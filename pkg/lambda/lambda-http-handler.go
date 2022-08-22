package lambda

import (
	"bytes"
	"encoding/base64"
	"encoding/json"
	"log"
	"net/http"
	"net/http/httptest"
	"os"
	"strings"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
)

func Start(server http.Handler) {
	lambda.Start(HttpAdapter(server))
}

func HttpAdapter(server http.Handler) func(events.LambdaFunctionURLRequest) (events.LambdaFunctionURLResponse, error) {
	return func(event events.LambdaFunctionURLRequest) (events.LambdaFunctionURLResponse, error) {
		if os.Getenv("DEBUG") == "true" {
			serialised, _ := json.Marshal(event)
			encoded := base64.StdEncoding.EncodeToString(serialised)
			log.Println("lambdarequest: " + encoded)
		}

		// read the body into a io reader - if its base64 encoded, decode it first
		var body []byte
		if event.IsBase64Encoded {
			b, err := base64.StdEncoding.DecodeString(event.Body)
			if err != nil {
				log.Println("base64 decoding failed")
				return events.LambdaFunctionURLResponse{StatusCode: http.StatusInternalServerError}, err
			}
			body = b
		} else {
			body = []byte(event.Body)
		}
		br := bytes.NewReader(body)

		requestHeaders := map[string][]string{}
		for k, v := range event.Headers {
			requestHeaders[capitalise(k)] = []string{v}
		}

		// create http request and response objects
		r := httptest.NewRequest(event.RequestContext.HTTP.Method, event.RawPath+"?"+event.RawQueryString, br)
		r.Header = requestHeaders

		if os.Getenv("DEBUG") == "true" {
			serialised, _ := json.Marshal(r.Header)
			encoded := base64.StdEncoding.EncodeToString(serialised)
			log.Println("httprequest headers:" + encoded)
		}

		w := httptest.NewRecorder()
		server.ServeHTTP(w, r)

		responseHeaders := map[string]string{}
		for k, v := range w.Header() {
			responseHeaders[k] = v[0]
		}

		encodedBody := base64.StdEncoding.EncodeToString(w.Body.Bytes())

		// convert the http response into a lambda response
		respEvent := events.LambdaFunctionURLResponse{
			Headers:         responseHeaders,
			IsBase64Encoded: true,
			Body:            encodedBody,
			StatusCode:      w.Code,
		}
		return respEvent, nil
	}
}

// while the http-standard specifies headers are case insensitive,
// apparently many languages including Go don't honour this :|
func capitalise(headerName string) string {
	res := ""
	parts := strings.Split(headerName, "-")
	for i, v := range parts {
		res += strings.ToUpper(v[0:1])
		if len(v) > 1 {
			res += v[1:]
		}
		if i != len(parts)-1 {
			res += "-"
		}
	}
	return res
}
