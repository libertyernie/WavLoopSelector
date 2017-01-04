WAV Loop Selector
=================

This is a GUI application (C# / Windows Forms) to edit the looping information in the smpl chunk of a WAV file.

Copyright © 2017 libertyernie

Based on code from BrawlBox (https://github.com/libertyernie/brawltools)
<br/>
Copyright © 2009-2016 Bryan Moulton, BlackJax96, Sammi Husky, libertyernie

This program is provided as-is without any warranty, implied or otherwise.
By using this program, the end user agrees to take full responsibility regarding its proper and lawful use.
The authors/hosts/distributors cannot be held responsible for any damage resulting in the use of this program, nor can they be held accountable for the manner in which it is used.

Normal usage:

    WavLoopSelector.exe [in-file] [out-file]

Read-only usage:

    WavLoopSelector.exe [in-file]

Both in-file and out-file can be '-' for stdin/stdout.

In read-only mode, the OK and Cancel buttons will be hidden and no changes will be saved.
