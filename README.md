# UNPACK ATELIER RYZA 3 EBM

A command-line tool for extracting and reinserting dialogs from [_Atelier Ryza 3_](https://store.steampowered.com/app/1999770/Atelier_Ryza_3_Alchemist_of_the_End__the_Secret_Key/)'s English .ebm files.

I coded this tool because Vita Smith's tool doesn't seem to be able to extract .ebm files from this particular game.

Notes
-----

You need `VitaSmith`'s "[_gust tools_](https://github.com/VitaSmith/gust_tools)" to open .pak files!
The .ebm files are in event/event_en/, in the "PACK01.ebm" file!

Building
========

In console, just type `dotnet build` (Powershell, VSC...)

Usage
=====

In console, type :

`UnpackEBM.exe filename.ebm path`

Example :

`UnpackEBM.exe event_message_mm01_010.ebm "event/event_en"`

Highly recommended :
Drag&drop your event_en folder in UnpackEBM.exe folder then execute `01. Extract story dialogues.bat`, all files will be extracted in .txt!

Translate .txt files, then execute `02. Insert story dialogues`, all files will be reinserted in .ebm!

Drag&drop the same event_en in your event/ folder, then repack PACK01.ebm, the game is translated!

License
=======

[GPLv3](https://www.gnu.org/licenses/gpl-3.0.html) or later.

Thanks
======

* VitaSmith for his [Gust Tools](https://github.com/VitaSmith/gust_tools) that make it extremely easy to modify Atelier games!
