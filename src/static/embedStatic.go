package main

import (
	"encoding/base64"
	"fmt"
	"io"
	"io/ioutil"
	"log"
	"os"
	"strings"
	"time"
)

const baseDir = "./static/"

func main() {
	fs, err := ioutil.ReadDir(baseDir)
	if err != nil {
		log.Fatal(err)
	}

	out, err := os.Create("embedded_statics.go")
	if err != nil {
		log.Fatal(err)
	}

	header := fmt.Sprintf("// generated by go generate and embedStatic.go; DO NOT EDIT\n// %s\npackage main\n\nfunc loadStatics() {\n", time.Now().Format(time.RFC3339))
	out.Write([]byte(header))

	for _, f := range fs {
		name := f.Name()
		if name != "embedStatic.go" {
			start := fmt.Sprintf("\tembeddedStatics[\"%s\"] = `", name)
			out.Write([]byte(start))

			if strings.HasSuffix(name, ".png") {
				f, _ := ioutil.ReadFile(baseDir + "/" + name)
				encoded := make([]byte, base64.StdEncoding.EncodedLen(len(f)))
				base64.StdEncoding.Encode(encoded, f)
				out.Write(encoded)
			} else {
				f, _ := os.Open(baseDir + "/" + name)
				defer f.Close()
				io.Copy(out, f)
			}

			out.Write([]byte("`\n"))
		}
	}
	out.Write([]byte("}\n"))
}
