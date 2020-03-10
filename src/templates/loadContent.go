package main

import (
	"fmt"
	"io"
	"io/ioutil"
	"log"
	"os"
	"strings"
)

const baseDir = "./templates/"

func main() {
	fs, err := ioutil.ReadDir(baseDir)
	if err != nil {
		log.Fatal(err)
	}

	out, err := os.Create("templates.go")
	if err != nil {
		log.Fatal(err)
	}

	master, err := ioutil.ReadFile(baseDir + "_master.html")
	if err != nil {
		log.Fatal(err)
	}

	out.Write([]byte("package main\n\nfunc loadTemplates() {\n"))
	for _, f := range fs {
		name := f.Name()
		if strings.HasSuffix(name, ".html") && name != "_master.html" {
			start := fmt.Sprintf("\ttemplateContent[\"%s\"] = `", name)
			out.Write([]byte(start))

			f, _ := os.Open(baseDir + "/" + name)
			io.Copy(out, f)

			out.Write(master)
			out.Write([]byte("`\n"))
		}
	}
	out.Write([]byte("}\n"))
}
