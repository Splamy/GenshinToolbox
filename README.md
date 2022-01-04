# GenshinToobox

## Features

### Automatic Artifacts Scraper

Reads all Artifacs and exports a json file in the [GOOD](https://frzyc.github.io/genshin-optimizer/#/doc) format

### Autofisher

Automatically hooks in and catches fishes.  
You only have to throw the hook yourself again :P

### Music Player

Plays Songs on the Harp or Lute.  
Can automatically parse [MusicXML](https://musescore.org/en/handbook/3/file-formats#musicxml) files, and postprocess them to fit into the 7-scale Keyboard of Genshin.

### Auto Expedition Collect and Redeploy

No 'Collect All and Redeploy' button? No problem.  
Collects all expeditions and starts them again with lightning speed.

## CLI

You can use the command line interface or call with `-h` to get help with the launch args to start the task you want directly.

# Publish

`dotnet publish -c Release --self-contained true`  
`dotnet publish -c Release --self-contained true -r win10-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true`