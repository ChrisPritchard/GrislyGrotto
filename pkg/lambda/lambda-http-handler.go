package lambda

import (
	"bytes"
	"encoding/base64"
	"log"
	"net/http"
	"net/http/httptest"

	"github.com/aws/aws-lambda-go/events"
	"github.com/aws/aws-lambda-go/lambda"
)

func Start(server http.Handler) {
	lambda.Start(HttpAdapter(server))
}

func HttpAdapter(server http.Handler) func(events.LambdaFunctionURLRequest) (events.LambdaFunctionURLResponse, error) {
	return func(event events.LambdaFunctionURLRequest) (events.LambdaFunctionURLResponse, error) {
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
			requestHeaders[k] = []string{v}
		}

		// create http request and response objects
		r := httptest.NewRequest(event.RequestContext.HTTP.Method, "/", br)
		r.Header = requestHeaders
		w := httptest.NewRecorder()
		server.ServeHTTP(w, r)

		responseHeaders := map[string]string{}
		for k, v := range w.Header() {
			responseHeaders[k] = v[0]
		}

		// convert the http response into a lambda response
		respEvent := events.LambdaFunctionURLResponse{
			Headers:    responseHeaders,
			Body:       w.Body.String(),
			StatusCode: w.Code,
		}
		return respEvent, nil
	}
}
