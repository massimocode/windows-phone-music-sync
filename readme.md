# Windows Phone Music Sync

## What does it do?
- It copies all folders and files inside the Windows User's Music Library to a folder called "From PC" inside the
Music folder on the Windows Phone.
- It does not copy files that already exist if they are the same size.
- It only copies 1 level of folders, so for example `Music Library/My Album` would be copied,
but `Music Library/My Album/Some Other Folder` would not.

## How do I use it?
1. Plug your phone into your computer
2. Make sure you've unlocked your phone
3. Open the Phone's Music folder in Windows Explorer - this is to ensure the computer can read/write to the phone
4. If this is the first time you're running the program, you will need to create a folder called "From PC"
(it is case-sensitive). This is where all your music will be synced to.
5. Run the program

## Why do I need to create a folder called "From PC". Why can't my music be synced directly to the Music folder?
We can create sub-folders inside the phone's Music folder using Windows Explorer but we get an error when trying to
do it via the WinRT API. Strangely enough, we only get an error when trying to create folders, not when trying to create files!
So the initial "From PC" subfolder needs to be created manually and then we will be able to sync whatever we like to that folder.

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