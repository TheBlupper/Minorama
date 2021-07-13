# Minorama

A project I made over a weekend to convert a chunk from a Minecraft 1.7.10 world to a QR-code scannable by the mobile game [Mekorama](https://play.google.com/store/apps/details?id=com.martinmagni.mekorama&hl=en&gl=US)

![image](https://user-images.githubusercontent.com/33233478/125460411-cfc31e8f-979c-451a-b5e1-9722cee65219.png)

## Installation

Just open the Visual studio project. You might need to relink the Substrate.dll.

You also need to install
- QRCodeEncoderLibrary.2.0.1
- SharpZipLib.1.3.2

from NuGet package manager

## Demonstration
![2021-07-13_15 03 16](https://user-images.githubusercontent.com/33233478/125460010-652c5ea8-e3e5-44e0-b1c3-639cf7c7f905.png)
This minecraft chunk can be selected via the program and converted into a QR code like this one
![test](https://user-images.githubusercontent.com/33233478/125460146-623224ea-d1f1-4ab5-916a-e1cf97b7ea50.png)

This can then be scanned by Mekorama and read in as a world
![Screenshot_20210713-150045_Mekorama](https://user-images.githubusercontent.com/33233478/125460232-b280795b-1802-4868-9e14-34ca0914d14d.jpg)

You might notice that the orientation of the pistons is a little off, and thats simply because I didn't bother fixing it. The oritentation id's work different for normal blocks and stairs and so I only had time to do one of those.

Anyway I hope someone finds this interesting. Don't comment on the messy C# code, it's not my primary language :)
