package embedded

import "embed"

//go:embed static/* templates/*
var Resources embed.FS
