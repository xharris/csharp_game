run:
	dotnet run -c Debug -r win-x64 -- D:\Documents\PROJECTS\dotnet_fun\mygame\main2.lua > output.txt

release:
	dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true