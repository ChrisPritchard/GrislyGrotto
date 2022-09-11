package main

import (
	"database/sql"
	"encoding/base64"
	"fmt"
	"log"
	"net/http"
	"net/url"
	"os"
	"strconv"
	"strings"

	"github.com/ChrisPritchard/GrislyGrotto/pkg/lambda"

	_ "modernc.org/sqlite"
)

const (
	authAccessKey = "ACCESSKEY"
	pageHTML      = `
		<!doctype html>
		<html>
			<head>
				<meta charset='utf-8'>
			</head>
			<body>
				<form method='POST'>
					<label>Connection string: <input type='text' name='connstring' value='' /></label>
					<br />
					<label>SQL<br/>
						<textarea rows=10 cols=50 name='sql'></textarea>
					</label>
					<br />
					<input type='submit' value='submit' />
				</form>
				<div id='result'></div>
			</body>
		</html>
		`
)

func main() {
	log.SetFlags(0)
	log.SetOutput(os.Stdout)

	lambda.Start(http.HandlerFunc(contentHandler))
}

func replace(r *http.Request, html, queryName, htmlToken string) string {
	if param := r.URL.Query().Get(queryName); param != "" {
		decoded, err := base64.URLEncoding.DecodeString(param)
		if err == nil {
			html = strings.Replace(html, htmlToken, htmlToken+string(decoded), 1)
		} else {
			log.Printf("error with query %s: %s\n", queryName, err.Error())
		}
	}
	return html
}

func contentHandler(w http.ResponseWriter, r *http.Request) {
	if r.URL.Query().Get(authAccessKey) != os.Getenv(authAccessKey) || r.FormValue(authAccessKey) != os.Getenv(authAccessKey) {
		http.Error(w, "missing or invalid credentials", http.StatusUnauthorized)
		return
	}

	if r.Method == "GET" {
		w.Header().Set("Content-Type", "text/html")
		html := pageHTML
		html = replace(r, html, "conn", "name='connstring' value='")
		html = replace(r, html, "lastsql", "name='sql'>")
		html = replace(r, html, "result", "id='result'>")
		w.Write([]byte(html))
		return
	}

	if r.Method != "POST" {
		http.NotFound(w, r)
		return
	}

	conn := r.FormValue("connstring")
	db, err := sql.Open("sqlite", conn)
	if err != nil {
		log.Println(err.Error())
		http.Error(w, "unable to open db with error:"+err.Error(), http.StatusBadRequest)
		return
	}

	sql := r.FormValue("sql")
	var res string
	if strings.HasPrefix(strings.ToLower(sql), "select") {
		rows, err := db.Query(sql)
		if err != nil {
			res = err.Error()
		} else {
			res = renderTable(rows)
		}
	} else {
		r, err := db.Exec(sql)
		if err != nil {
			res = err.Error()
		} else {
			rows, _ := r.RowsAffected()
			res = fmt.Sprintf("exec successful, %d rows affected", rows)
		}
	}

	url := fmt.Sprintf("/?ACCESSKEY=%s&conn=%s&lastsql=%s&result=%s",
		os.Getenv(authAccessKey),
		url.QueryEscape(base64.URLEncoding.EncodeToString([]byte(conn))),
		url.QueryEscape(base64.URLEncoding.EncodeToString([]byte(sql))),
		url.QueryEscape(base64.URLEncoding.EncodeToString([]byte(res))))
	http.Redirect(w, r, url, http.StatusFound)
}

func renderTable(rows *sql.Rows) string {

	res := "<table><thead><tr>"
	cols, _ := rows.Columns()
	for _, v := range cols {
		res += "<td>" + v + "</td>"
	}
	res += "</tr></thead><tbody>"

	scanData := make([]interface{}, len(cols))
	types, _ := rows.ColumnTypes()
	for i, v := range types {
		switch v.DatabaseTypeName() {
		case "BOOL":
			scanData[i] = new(sql.NullBool)
		case "INT4":
			scanData[i] = new(sql.NullInt64)
		default:
			scanData[i] = new(sql.NullString)
		}
	}

	for rows.Next() {
		rows.Scan(scanData...)
		res += "<tr>"
		for _, v := range scanData {
			c := ""
			if vc, ok := v.(*sql.NullString); ok {
				c = vc.String
			} else if vc, ok := v.(*sql.NullInt64); ok {
				c = strconv.Itoa(int(vc.Int64))
			} else if vc, ok := v.(*sql.NullBool); ok {
				c = "FALSE"
				if vc.Bool {
					c = "TRUE"
				}
			}
			res += "<td>" + c + "</td>"
		}
		res += "</tr>"
	}

	res += "</tbody></table>"
	return res
}
