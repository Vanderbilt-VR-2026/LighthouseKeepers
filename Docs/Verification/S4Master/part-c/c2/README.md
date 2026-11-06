# C2 material pass

CC0 stone, timber, painted iron and rock maps imported; derived brass and limewash variants documented in AssetSources.md. Existing material GUIDs preserved, shared URP Lit materials use instancing, ASTC and mipmaps. Shell surfaces use planar metre UVs (2 m per tile); existing lightmap UVs and collision are retained. Superseded unreferenced generated UV copies removed before commit; authoring skips already-projected meshes.

House/pipe/floor previews reviewed. Initial stone was too dark; final limewash preview is brighter while keeping damp cracks. All-scene coplanar regression remains zero. GPU cost, texture shimmer and device readability remain pending. Powered lighting still uses the previous bake until C3.
