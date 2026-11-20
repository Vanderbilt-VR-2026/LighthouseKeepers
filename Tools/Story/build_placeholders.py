#!/usr/bin/env python3
"""macOS offline TTS for SHEET 0 only. Never overwrite an imported performance.
No playback; say writes to disk. Sources/transients are kept out of the build.
"""
from pathlib import Path
import argparse,concurrent.futures,hashlib,json,re,subprocess,uuid,wave
ROOT=Path(__file__).resolve().parents[2]
def main():
 p=argparse.ArgumentParser();p.add_argument('--verify',action='store_true');args=p.parse_args()
 catalogue={e['id']:e for e in json.loads((ROOT/'Assets/Data/Strings/Resources/StoryVoiceCatalog.json').read_text())['entries']}
 manifest=json.loads((ROOT/'Assets/Data/Strings/Resources/StoryVoiceAvailability.json').read_text())
 sheet=(ROOT/'Docs/Story/03-Recording-Sheets.md').read_text().split('# OPTIONAL LINES')[0]
 rows=re.findall(r'^\| \d+ \| `(\w+)\.wav` \| (.*?) \|',sheet,re.M)
 assert len(rows)==52 and {x.upper() for x,_ in rows}==set(manifest['selected'])
 for name,text in rows:assert catalogue[name.upper()]['text']==text,(name,'Caption mismatch')
 receipt=ROOT/'Docs/Story/placeholder-manifest.json'
 old={x['id']:x for x in json.loads(receipt.read_text())['clips']} if receipt.exists() else {}
 cache=ROOT/'artifacts/s5/loop/tts';cache.mkdir(parents=True,exist_ok=True)
 ffmpeg=Path.home()/'.local/bin/ffmpeg'
 def render(row):
  name,text=row;entry=catalogue[name.upper()];dest=ROOT/entry['file'];target=-24 if entry['id'].startswith('ANT_') else -18
  if entry['id'] in manifest['recorded']:return {'id':entry['id'],'source':'recording','file':entry['file']}
  if not args.verify:
   if dest.exists():
    assert entry['id'] in old and hashlib.sha256(dest.read_bytes()).hexdigest()==old[entry['id']]['sha256'],'Unrecognized existing audio; preserve '+name
   else:
    dest.parent.mkdir(parents=True,exist_ok=True)
    source=cache/(name+'.aiff');subprocess.run(['say','-v','Daniel','-r','168','-o',str(source),text],check=True)
    chain='highpass=f=80,afftdn=nr=6:nf=-40,deesser=i=0.15:m=0.35:f=0.5,equalizer=f=2800:t=q:w=0.8:g=2.5,silenceremove=start_periods=1:start_duration=0.02:start_threshold=-45dB:start_silence=0.08,areverse,silenceremove=start_periods=1:start_duration=0.02:start_threshold=-45dB:start_silence=0.08,areverse,acompressor=threshold=0.125:ratio=2:attack=10:release=80'
    norm=f'loudnorm=I={target}:TP=-1.5:LRA=7'
    stats=subprocess.run([str(ffmpeg),'-hide_banner','-nostdin','-i',str(source),'-af',chain+','+norm+':print_format=json','-f','null','-'],capture_output=True,text=True,check=True).stderr
    measured=json.JSONDecoder().raw_decode(stats[stats.rindex('{'):])[0]
    norm+=':measured_I='+measured['input_i']+':measured_TP='+measured['input_tp']+':measured_LRA='+measured['input_lra']+':measured_thresh='+measured['input_thresh']+':offset='+measured['target_offset']+':linear=true'
    subprocess.run([str(ffmpeg),'-hide_banner','-loglevel','error','-nostdin','-y','-i',str(source),'-af',chain+','+norm,'-ac','1','-ar','44100','-c:a','pcm_s16le',str(dest)],check=True)
    Path(str(dest)+'.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'''\nAudioImporter:
  serializedVersion: 7
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 0.8
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {}
  forceToMono: 1
  normalize: 0
  loadInBackground: 0
  ambisonic: 0
  3D: 1
''')
  if not args.verify:
   def measured_output():
    log=subprocess.run([str(ffmpeg),'-hide_banner','-nostdin','-i',str(dest),'-af','loudnorm=print_format=json','-f','null','-'],capture_output=True,text=True,check=True).stderr
    return json.JSONDecoder().raw_decode(log[log.rindex('{'):])[0]
   actual=measured_output();gain=target-float(actual['input_i'])
   if abs(gain)>.2:
    corrected=cache/(name+'-level.wav')
    subprocess.run([str(ffmpeg),'-hide_banner','-loglevel','error','-nostdin','-y','-i',str(dest),'-af',f'volume={gain}dB,aresample=176400,alimiter=limit=0.8414:level=0:latency=1,aresample=44100','-ac','1','-ar','44100','-c:a','pcm_s16le',str(corrected)],check=True)
    dest.write_bytes(corrected.read_bytes())
  with wave.open(str(dest),'rb') as w:
   duration=w.getnframes()/w.getframerate();assert w.getnchannels()==1 and w.getframerate()==44100 and w.getsampwidth()==2
   assert 0.2<duration<10,(name,duration)
  return {'id':entry['id'],'source':'macOS Daniel offline TTS; temporary placeholder','file':entry['file'],'duration':round(duration,3),'targetLUFS':target,'sha256':hashlib.sha256(dest.read_bytes()).hexdigest()}
 with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:clips=list(pool.map(render,rows))
 if not args.verify:receipt.write_text(json.dumps({'clips':clips},indent=2)+'\n')
 print(f'{len(clips)} selected single takes, mono44.1kHz16bit, all under10s; optional recordings preserved.')
if __name__=='__main__':main()
