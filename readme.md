# Windows Phone Music Sync

## What does it do?
- It copies all folders and files inside the Windows User's Music Library to a folder called "From PC" inside the
Music folder on the Windows Phone.
- It does not copy files that already exist if they are the same size.
- It only copies 1 level of folders, so for example `Music Library/My Album` would be copied,
but `Music Library/My Album/Some Other Folder` would not.

## How do I use it?
Before you run the program, it might be a good idea to make a backup of your phone, just in case this does something crazy.
It works for me though :)
1) Plug your phone into your computer
2) Make sure you've unlocked your phone
3) Open the Phone's Music folder in Windows Explorer - this is to ensure the computer can read/write to the phone
4) If this is the first time you're running the program, you will need to create a folder called "From PC"
(it is case-sensitive). The reason you need to create this folder is that the normal "Music" folder is "read-only"
and I haven't found a way to allow C# to create subfolders within it, even though we can create files in it!
So the initial subfolder needs to be created manually and then we will be able to sync whatever we like to that folder.
5) Run the program

## The program doesn't work
Try sending me a screenshot of the issue you're having and I'll try to fix it when I get some time.

## The story behind this program
After searching the internet for hours for a way to synchronise the music on my Windows 10 computer to my Windows
10 mobile, I was left disappointed.

There was talk of a Windows Phone application on Windows 8, but nothing for Windows 10.

The only options are Copy/Paste (which means overwriting everything if you don't know what's changed),
or uploading everything to OneDrive and using that (which isn't really an option if you have a very large library,
and is slow as hell).

So I thought to myself "hey, how hard could it be to write something in C#?"

Turns out, standard .NET stuff does not support reading/writing to the Windows Phone filesystem, and it does not
show up as a removable drive.

So I googled and googled and googled some more and found that it's possible to do with the WinRT stuff. So I coded this
application to use the WinRT APIs and it took me a couple of hours but it does exactly what I wanted it to do.