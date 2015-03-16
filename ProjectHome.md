# PreNIS - The pre-processor for integrating .Net with the NullSoft Install System #

PreNIS is a preprocessor macro language extension to the NSIS scripting language to automatically include dlls, exes or project version information in to NSIS scripts.

## Why make a Pre-Processor for NSIS? ##

At Pixolüt, we have been using the Nullsoft Install System for a long time. We rely on it with every build pipeline for .Net user applications and also server deployments. They all benefit from the flexibility NSIS offers.

The only problem with using any external installer system, especially a script based one is that its painful to manage the sync between the project file and the installer script to ensure that only required DLLs and content files are included - and every time a new DLL, file or folder is added to the project, the install script needs to be updated.

Then there is the issue of version information - that needs to come from the assembly info file!

Wouldn't it be great to be able to use the .Net project files to dynamically create the NSI file whilst still having all the power and features of NSIS? Well, with PreNIS you can. PreNIS provides a simple set of macro tags which will expand out and repeat for all folders or files specified and create a new NSI file which contains all correct files in the project.

  * [Summary of how to use PreNIS](http://code.google.com/p/prenis/wiki/Usage)
  * [Examples](http://code.google.com/p/prenis/wiki/Examples).
  * [Download latest version of PreNIS](http://code.google.com/p/prenis/downloads/list).
