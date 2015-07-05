**Introduction**

Models in SM64DS often make use of bones/joints to allow the model to be animated. Each vertex is assigned to a joint which is
then animated separately. Within the BMD model each vertex is stored with the inverse of its bone's transformation applied (
the "inverse bind pose"). Each vertex must have its bone's transformations applied to it for it to get its final position. For
animation the transformations are applied to the vertex in its default state ie. with no transformations applied.

SM64DSe supports exporting models to either OBJ or DAE (recommended). Models exported to DAE have joints and skinning included
and are near-identical to the original BMD model.

(Please see BMD documentation for more information.)

**Exporting COLLADA DAE - Recommended**

When exporting to DAE the model will be exported with full joints and skinning, with each vertex assigned to a joint (vertex weight of 1.0).
This means that the exported model should be an exact match to the BMD model and when re-importing models as all vertices will be part of
the same mesh, vertices with different joints remain connected meaning animation will work smoothly and exactly the same as with the original BMD model.

This is the recommended format for exporting as it matches the original BMD almost exactly.

**Exporting OBJ**

When exporting, the bones' transformations are applied so the model looks 'normal' when editing. The OBJ format does not support
joints and skinning. Instead, the model is split up into its different chunks using the OBJ "o" command. Because of this, each vertex
can only be assigned on a per-face basis, meaning that some vertices are assigned to the wrong bone and animation will not work
smoothly when re-importing as vertices with different bones are not connected.

Information about each bone is stored in the .bones file created when exporting. This is linked to the .obj
file with the line "bonelib FILENAME.bones" at the top of the .obj file. <br>
The .bones file contains the following<br>
data on each bone:<br>
<i>newbone</i>: this tells the importer a new bone is being defined and gives the bone name, like the newmtl command in<br>
the material library<br>
<i>parent_offset</i>: Offset in bones the current bone's parent<br>
<i>has_children</i>: Whether or not the bone has child bones, 0 - no, 1 - yes<br>
<i>sibling_offset</i>: Offset in bones to the current bone's next sibling. <br>These three fields are used to determine the<br>
bones' hierarchical structure<br>
<i>scale</i>: X, Y and Z Scale transformation of the current bone. (32-bit signed, 20:12 fixed point. Think GX command 0x1B)<br>
<i>rotation</i>: X, Y and Z Rotation transformation of the current bone. (16-bit signed, 0x0400 = 90Â°)<br>
<i>translation</i>: X, Y and Z Translation of the current bone. (32-bit signed, 20:12 fixed point. Think GX command 0x1C)<br>
These last three fields are stored in hexadecimal format.<br>
<br>
Important Notes:<br>
- Please note that when exporting to OBJ vertices are assigned to a bone on a per-face basis, unlike BMD and DAE where<br>
each individual vertex is assigned to a bone.</br>


<b>Importing COLLADA DAE - Recommended</b>

SM64DSe allows importing COLLADA DAE models with joints and skinning. The skeleton attached to a mesh is used to create the<br>
bones in the BMD model and the vertex weighting is used to assign each individual vertex to a bone. This allows an imported<br>
DAE model to match the original BMD model exactly and animation will work exactly the same as before.<br>
<br>
SM64DSe supports importing models with transformations defined as either separate Scale, Rotation and Translation values (preferred)<br>
or using SRT 4x4 matrices. Transformation matrices that have been multiplied in any other order will not produce correct results.<br>
<br>
</br>

<b>Importing OBJ</b>

When importing, the bones' transformations are applied in reverse so that they are restored to their original state and<br>
their transformations can be applied again. For OBJ the OBJ "o" command is used to split the model into bones. To specify<br>
the settings for each bone, you must include a .bones file, for OBJ the following line at the beginning of the model is<br>
used:<br>
bonelib FILENAME.bones<br>
<br>
where FILENAME is the name of your .bones file.<br>
<br>
A .bones file is not necessary, however models that use animation will not be animated correctly as the animation file (.BCA)<br>
expect certain transformations to be applied to the model. If not found, each bone will be assigned the default values with each<br>
bone made a root bone.<br>
<br>
When creating a new OBJ model, you can either edit an exported OBJ model, already split up into separate objects, or you can split up an<br>
existing model in a similar manner to original.<br>
<br>
- Once finished editing/creating your model, export it to OBJ format(for OBJ your modelling program must support the OBJ "o" command).<br>
- Ensure the bone names in your model match those listed in the .bones file, if they don't, you will need to manually change<br>
either your model or .bones file<br>
- Add the .bones file to your model as described above.<br>
- Import your model<br>
<br>
<b>Tips on texture sequence animations (BTP)</b>

Some models eg. the characters have textures animated using .BTP files (sequence of textures to swap) such as Mario blinking.<br>
Whilst you could find every BTP file and manually change the texture, palette and material names, it's much easier to give your<br>
new model's materials, textures and palettes the same name. Material names are set using the name defined in your model,<br>
texture names are set to the diffuse map filename (leave out the extension to get a match to the original) and palettes are the<br>
texture name + "