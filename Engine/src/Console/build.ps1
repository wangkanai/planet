rm tiler
rimraf bin
dotnet build   -tl
dotnet publish -tl
cp ./bin/Release/net9.0/osx-arm64/publish/tiler .
