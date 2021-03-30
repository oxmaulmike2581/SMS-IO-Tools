# SMS-IO-Tools
Tools for working with models and textures in Madness Engine by Slightly Mad Studios

At this moment this repository contains a two tools:
* MEB File Patcher
* TEX File Patcher

First one need to replace the header in the `*.meb` files.<br>
Second one used to remove unneccessary header in the `*.tex` files and auto-rename it to `*.dds` (only for PC version at this moment)<br>

Usage:<br>
* Process a single file:<br>
`Just drag and drop the file you need to the EXE file of the tool`
* Batch processing via `*.cmd` file:<br>
`for %%a in (*.meb) do MEBFilePatcher.exe %%a`<br>
`for %%a in (*.tex) do TEXFilePatcher.exe %%a`<br>

TODO:
* Find a PS4 SDK to get a library to work with GNF images (I know about Noesis but want to build a standalone converter)
* Find more information about MEB, BML, BMT, VHF structure (More than we have now)
* Find a encryption method used in some vertex coordinates (I think we need to research the shaders)

More information about these formats (researched by me; will be updated if more information is given) can be found here:
https://docs.google.com/document/d/12vAnWQXt_Ohd7yoUw0nr1P5y33R4yuO3C66AHtPLTU8/edit?usp=sharing

Some references from other authors:
* BFF structure: https://forum.xentax.com/viewtopic.php?p=31863#p31863
* BML/BMT structure and converter: http://projects.pappkartong.se/bmt2xml/
* SGB structure and converter: http://projects.pappkartong.se/sgbconverter/

Tools in this repo are written in .NET C# and requires .NET Framework 4.7.2.

Sorry for the mistakes in the text, my English is not perfect.
