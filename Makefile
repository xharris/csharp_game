run:
	dotnet run -c Debug -r win-x64 > output.txt

release:
	dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true