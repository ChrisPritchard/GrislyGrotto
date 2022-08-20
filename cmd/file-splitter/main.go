package main

import (
	"flag"
	"fmt"
	"io"
	"log"
	"os"
	"path/filepath"
)

func main() {
	log.SetFlags(0)

	fullFilename := flag.String("f", "", "file to split")
	size := flag.Int("k", 5000, "size in kilobytes of each chunk")
	flag.Parse()

	file, err := os.Open(*fullFilename)
	if err != nil {
		log.Fatal(err)
	}
	defer file.Close()

	filename := filepath.Base(*fullFilename)
	buffer := make([]byte, *size*1024)
	i := 0
	read, readErr := file.Read(buffer)
	for readErr != io.EOF {
		i++
		folder := fmt.Sprintf("%s.%d", filename, i)
		os.Mkdir(folder, 0755)
		os.WriteFile(folder+"/"+filename, buffer[0:read], 0644)
		read, readErr = file.Read(buffer)
	}

	log.Printf("done - %d parts written\n", i)
}
