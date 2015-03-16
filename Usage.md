## 1. Basic Usage ##

```
prenis.exe "-source=c:\my project\installer\install.nsi" -target=c:\output.nsi

-source
source (input) nsi file

-target
target (output) nsi file

-help
shows a simple help text
```

To use quotes around a filename, make sure you use quotes around the argument too
"-source=bah bah" is correct
-source="bah bah" is not correct

## 2. Script Documentation ##
All commands follow the format:
##command##
where they start and end with ## to specify the preprocessor arguments.
Arguments are sperated by the ; character and use argument=value format.
Arguments can be split over multiple lines

```
##command;
 arg=value;
 arg2=value##
```

### The Import command ###

```
##import;
 project=filename of project file;
 folder=path to project files;
 config=Debug##
```

The folder argument is optional and not used if the project file is in the base folder of the project file.


### Macro commands ###
In between a macro, you can put any NSI commands and use the macro-variables.
The macro-variables work as follows:

```
%%absfilename%%  
insert the absolute filename ie: c:\project\myfile.txt

%%abspath%%
insert the absolute path ie: c:\project

%%outpath%%
insert the relative path ie: project

%%filename%%
insert the filename only ie: myfile.txt

%%relfilename%%
insert the relative path and filename 
ie: project\myfile.txt which is equivalent 
to %%outpath%%%%filename%%
```

**macro.dll** will insert all DLLs for the specified assembly. Ensure all dependant projects are also imported!

```
##macro.dll;assembly=assembly name##
 %%absfilename%%
 %%abspath%%
 %%outpath%%
 %%filename%%
 %%relfilename%%
##macro.end##
```

You can control how the DLL macro operates by using the following optional arguments:
includeProjects = [true|false]
• this will include all DLLs which are from the project(s).
includeReferences = [true|false]
• this will choose wether you include the referenced DLLs
recursive = [true|false]
• this will set the recursion through all dependancies capability.

```
##macro.dll;
includeProjects=false;
includeReferences=true;
recursive=true;
assembly=assembly name##
 %%absfilename%%
 %%abspath%%
 %%outpath%%
 %%filename%%
 %%relfilename%%
##macro.end##
```

**macro.pdb** will insert all PDB files for the specified assembly. Ensure all dependant projects are also imported!

```
##macro.pdb;assembly=assembly name##
 %%absfilename%%
 %%abspath%%
 %%outpath%%
 %%filename%%
 %%relfilename%%
##macro.end##
```

**macro.contentfolders** will iterate through all folders for the specified project.
You can insert the macro.content macro inside a macro.contentfolders macro to expand out for each content file in each directory. the `%%filename%%` and `%%abspath%%` variables will act strange inside the contentfolders macro since they specify folders not files.

```
##macro.contentfolders;assembly=assembly name##
 %%absfilename%%
 %%abspath%%
 %%outpath%%
 %%filename%%
 %%relfilename%%
 ##macro.content##
  %%absfilename%%
  %%abspath%%
  %%outpath%%
  %%filename%%
  %%relfilename%%
 ##macro.end##
##macro.end##
```

**macro.content** will iterate through all files in the project if not used inside a contentfolders macro

```
##macro.content;assembly=assembly name##
 %%absfilename%%
 %%abspath%%
 %%outpath%%
 %%filename%%
 %%relfilename%%
##macro.end##
```

You can exclude or include specific files using the exclude or include commands inside a macro. The names allow wildcards either **name, name** or **name** to specifiy part of a filename. You can only use exclude or include exclusively, since by using the include command will automatically EXCLUDE ALL FILES except the files included. The Exclude command with INCLUDE ALL FILES except the ones excluded by the exclude commands.

You can have multiple of the same kind inside a single macro.

```
##exclude;filename=*config*##
##include;filename=config*##
```

The **version** command will take the version number from the assemblyinfo.cs (assemblyinfo.vb) for the specified assembly. The assembly must be imported first.

```
##version;assembly=assembly name;delimiter=.##
```