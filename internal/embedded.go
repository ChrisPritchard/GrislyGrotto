package internal

import "embed"

//go:embed static/*
var embeddedStatic embed.FS

//go:embed templates/*
var embeddedTemplates embed.FS
