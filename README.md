# JAFU-Music-Bot

A music bot for discord, dependent on DSharpPlus and Lavalink libraries for communicating with discord and youtube API

You need JAVA 13 and later in order to run Lavalink server

## Features
* Play music in a specific voice channel 
* Search music on youtube directly from the discord commad
* Play a random song
* Queue system to play the songs

## Available commands
* ! is the prefix used to call the commands
* !greet : a greeting with the callers name
* !random x y : returns a random number between x and y
* !play name : joins the specified channel for music and checks if the song is in the playlist, if not it adds it the the end. If there is no song playing then it plays the specific song.  
* !track : return information about the current song playing
* !pause : pauses the music playback
* !resume : resumes the playback
* !replay : replays the playlist form the first song
* !add : add a song to the playlist
* !repeate : enables or disables the auto replay feature
* !playlist : lists the songs currently playing in the playlist

## ToDo
* Add a shuffel abilty 
* Add an integration with spotify API

To use you have to create a settings.json file with two keys-value pairs:
1. token your token from discord API
2. channel the voice channel you want your bot to connect to when playing
