// Dark Dungeon Tileset // 


Grid
Dark Dungeon Tileset is built on a 6mx6m grid. Most tiles are this size, there are also a few half tiles 3mx6m. If you are planning to place the tiles down manually it is recommended that you change Unity’s default snapping settings to 3 and rotational snap to 90. This can be found in the grid and snap settings (Edit --> Grid and Snap Settings). Use ctrl when moving and rotating to snap them into place efficiently. Done this way even large levels can be put together quickly. Remember to keep an eye on the transform data and always keep to whole numbers, this is the easiest way to see what tiles have shifted off-grid and the ones that are ok.

Nested Prefabs
Included with the pack are several ready-made rooms. It is recommended to use this method when constructing larger levels. Begin by building a set of small base rooms then use those to generate further levels.

Shaders
All shaders were built using Unity’s Shader Graph, these can be easily opened and edited as seen fit. 

Fog Shader
This shader requires the depth texture to be rendered by the Main camera. This can be turned on in the render pipeline settings. The default location of this is – Assets/Settings/UniversalRenderPipelineAsset
Included are 2 versions of the shader, one for perspective cameras and the other for orthographic cameras. If you see a solid colour on your fog volume it is likely that you are using the wrong one for the type of camera. Note: The scene camera is perspective by default.

Light Mapping
This pack was designed with light mapping in mind to capture the glows from the wall at their best. Unity’s default gpu baker does a great job. 



Thank you for purchasing Dark Dungeon Tile Set, please consider leaving a review and checking out my other packs!

https://assetstore.unity.com/publishers/314


// For questions and queries please contact me here: matt.nv3d@gmail.com //