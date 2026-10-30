// Encode five seconds of the host keeper's first-person view without overlays.
// swift tools/NetworkingProof/encode.swift HOST_DIR OUTPUT.mp4
import Foundation
import AVFoundation
import CoreGraphics
import ImageIO
import CoreVideo

let args = CommandLine.arguments
guard args.count == 3 else { fatalError("Expected host frames and output.mp4") }
let fps = 15
let width = 1280, height = 720
func frames(_ dir: String) throws -> [(Int, URL)] {
    try FileManager.default.contentsOfDirectory(atPath: dir).compactMap { name in
        guard name.hasPrefix("frame-"), name.hasSuffix(".png"), let n = Int(name.dropFirst(6).dropLast(4)) else { return nil }
        return (n, URL(fileURLWithPath: dir).appendingPathComponent(name))
    }.sorted { $0.0 < $1.0 }
}
let host = try frames(args[1])
guard let h0 = host.first, let h1 = host.last else { fatalError("Missing captures") }
let first = h0.0, last = h0.0 + fps * 5 - 1
guard h1.0 >= last else { fatalError("Fewer than five seconds of captured frames") }
let output = URL(fileURLWithPath: args[2])
let writer = try AVAssetWriter(outputURL: output, fileType: .mp4)
let input = AVAssetWriterInput(mediaType: .video, outputSettings: [
    AVVideoCodecKey: AVVideoCodecType.h264, AVVideoWidthKey: width, AVVideoHeightKey: height,
    AVVideoCompressionPropertiesKey: [AVVideoAverageBitRateKey: 6_000_000, AVVideoMaxKeyFrameIntervalKey: fps * 2]
])
input.expectsMediaDataInRealTime = false
let adaptor = AVAssetWriterInputPixelBufferAdaptor(assetWriterInput: input, sourcePixelBufferAttributes: [
    kCVPixelBufferPixelFormatTypeKey as String: kCVPixelFormatType_32ARGB,
    kCVPixelBufferWidthKey as String: width, kCVPixelBufferHeightKey as String: height,
    kCVPixelBufferCGImageCompatibilityKey as String: true, kCVPixelBufferCGBitmapContextCompatibilityKey as String: true
])
writer.add(input)
guard writer.startWriting() else { fatalError("Cannot start: \(String(describing: writer.error))") }
writer.startSession(atSourceTime: .zero)
var hi = 0
var cached: [URL: CGImage] = [:]
func image(_ url: URL) -> CGImage {
    if let found = cached[url] { return found }
    guard let source = CGImageSourceCreateWithURL(url as CFURL, nil), let img = CGImageSourceCreateImageAtIndex(source, 0, nil) else { fatalError("Cannot read \(url)") }
    cached[url] = img; return img
}
for frame in first...last {
    while hi + 1 < host.count && host[hi + 1].0 <= frame { hi += 1 }
    while !input.isReadyForMoreMediaData {
        if writer.status == .failed { fatalError("Writer: \(String(describing: writer.error))") }
        Thread.sleep(forTimeInterval: 0.002)
    }
    autoreleasepool {
        var maybeBuffer: CVPixelBuffer?
        let result = CVPixelBufferPoolCreatePixelBuffer(kCFAllocatorDefault, adaptor.pixelBufferPool!, &maybeBuffer)
        guard result == kCVReturnSuccess, let buffer = maybeBuffer else { fatalError("Cannot allocate pixel buffer") }
        CVPixelBufferLockBaseAddress(buffer, [])
        guard let context = CGContext(data: CVPixelBufferGetBaseAddress(buffer), width: width, height: height,
            bitsPerComponent: 8, bytesPerRow: CVPixelBufferGetBytesPerRow(buffer), space: CGColorSpaceCreateDeviceRGB(),
            bitmapInfo: CGImageAlphaInfo.noneSkipFirst.rawValue) else { fatalError("Cannot create context") }
        context.interpolationQuality = .high
        context.draw(image(host[hi].1), in: CGRect(x: 0, y: 0, width: width, height: height))
        CVPixelBufferUnlockBaseAddress(buffer, [])
        guard adaptor.append(buffer, withPresentationTime: CMTime(value: Int64(frame - first), timescale: Int32(fps))) else { fatalError("Append: \(String(describing: writer.error))") }
    }
    cached = cached.filter { $0.key == host[hi].1 }
}
input.markAsFinished()
writer.endSession(atSourceTime: CMTime(value: Int64(last - first + 1), timescale: Int32(fps)))
let done = DispatchSemaphore(value: 0)
writer.finishWriting { done.signal() }
done.wait()
guard writer.status == .completed else { fatalError("Finish: \(String(describing: writer.error))") }
print("Wrote \(output.path): \(last - first + 1) frames, \(Double(last-first+1)/Double(fps)) seconds, \(width)x\(height).")
// Decode representative exported frames as a check of the actual MP4, not just source PNGs.
let asset = AVURLAsset(url: output)
let generator = AVAssetImageGenerator(asset: asset)
generator.appliesPreferredTrackTransform = true
generator.requestedTimeToleranceBefore = .zero
generator.requestedTimeToleranceAfter = .zero
for seconds in [1, 3, 4] {
    let cgFrame = try generator.copyCGImage(at: CMTime(seconds: Double(seconds), preferredTimescale: 600), actualTime: nil)
    let destination = output.deletingLastPathComponent().appendingPathComponent("video-check-\(seconds)s.png")
    let png = CGImageDestinationCreateWithURL(destination as CFURL, "public.png" as CFString, 1, nil)!
    CGImageDestinationAddImage(png, cgFrame, nil)
    guard CGImageDestinationFinalize(png) else { fatalError("Cannot export decoded video frame") }
}
print("Decoded and checked MP4 frames at 1, 3 and 4 seconds.")
