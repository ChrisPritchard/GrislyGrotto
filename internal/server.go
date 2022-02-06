package internal

import (
	"context"
	"log"
	"net/http"

	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
	"github.com/ChrisPritchard/GrislyGrotto/internal/handlers"
	"github.com/ChrisPritchard/GrislyGrotto/pkg/cookies"
)

func StartServer() {
	proceed := config.ParseArgs() // setup globals from cmd line flags and files
	if !proceed {
		return
	}

	handlers.SetupRoutes()

	server := globalHandler(http.DefaultServeMux)

	log.Printf("The Grisly Grotto has started!\nlistening locally at port %s\n", config.ListenURL)
	log.Println(http.ListenAndServe(config.ListenURL, server))
}

func globalHandler(h http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {

		// set security headers
		headers := w.Header()
		headers.Set("X-Frame-Options", "SAMEORIGIN")
		headers.Set("X-XSS-Protection", "1; mode=block")
		headers.Set("X-Content-Type-Options", "nosniff")
		headers.Set("Strict-Transport-Security", "max-age=31536000; includeSubDomains")
		headers.Set("Referrer-Policy", "same-origin")

		csp := "default-src 'self';"
		csp += "style-src 'self' 'unsafe-inline';"
		csp += "frame-src 'self' *.youtube.com;"
		headers.Set("Content-Security-Policy", csp)

		user, _ := cookies.ReadEncryptedCookie("user", config.Secret, config.AuthSessionExpiry, r)
		var userVal *string
		if user != "" {
			userVal = &user
			cookies.SetEncryptedCookie("user", user, config.Secret, config.AuthSessionExpiry, w)
		}

		userCtx := context.WithValue(r.Context(), config.AuthenticatedUser, userVal)
		h.ServeHTTP(w, r.WithContext(userCtx))
	})
}
