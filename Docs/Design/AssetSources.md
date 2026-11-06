# Visual pass asset sources

All four PBR sets are Poly Haven assets under CC0: https://polyhaven.com/license . Downloaded from the official files API, 5 November 2026.

| Surface | Source | Resolution |
|---|---|---|
| Salt-dark plaster/stone | https://polyhaven.com/a/plastered_stone_wall | 2K |
| Worn timber | https://polyhaven.com/a/weathered_planks | 1K |
| Rusty painted iron | https://polyhaven.com/a/green_metal_rust | 1K |
| Rock / wet ground | https://polyhaven.com/a/rock_boulder_dry | 1K |

Diffuse and OpenGL normal maps are original downloads. Packed mask is derived from ARM: metallic→R, AO→G, zero→B, inverse roughness→A. Colour/masks use ASTC6x6, normals ASTC5x5 on Android; trilinear mipmaps and anisotropy4. No displacement or parallax. Brass is a tinted metal variant, not a claimed brass photoscan. Existing material GUIDs retained. All later generated meshes, water normals, haze and sound effects are original project code, not third-party downloads.

Plaster diffuse is desaturated and blended 60% toward a pale limewash; source normal is retained at 0.30 strength. Brass diffuse is a brightened grayscale metal derivative with a 0.86 metallic mask. Untouched downloaded originals remain in local artifacts.
