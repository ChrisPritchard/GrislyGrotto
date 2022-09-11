package cookies

import (
	"crypto/aes"
	"crypto/cipher"
	"crypto/rand"
	"encoding/base64"
	"errors"
	"io"
	"net/http"
	"strings"
	"time"

	"github.com/ChrisPritchard/GrislyGrotto/internal/config"
)

func SetCookie(name, data string, expires time.Time, w http.ResponseWriter) {
	cookie := http.Cookie{
		Name:     name,
		Value:    data,
		HttpOnly: true,
		Expires:  expires,
		Path:     "/",
		Secure:   true,
		SameSite: http.SameSiteStrictMode,
	}
	http.SetCookie(w, &cookie)
}

func SetEncryptedCookie(name, unencodedData string, secret [16]byte, lifeTime time.Duration, w http.ResponseWriter) (err error) {
	now := config.CurrentTime()
	data := now.Format(time.RFC3339) + "|" + unencodedData
	cipher, err := encrypt([]byte(data), secret[:])
	if err != nil {
		return err
	}
	b64 := base64.StdEncoding.EncodeToString(cipher)

	SetCookie(name, b64, now.Add(lifeTime), w)
	return nil
}

func ReadEncryptedCookie(name string, secret [16]byte, allowedLifeTime time.Duration, r *http.Request) (unencodedData string, err error) {
	cookie, err := r.Cookie(name)
	if err != nil {
		return "", err
	}

	b64 := cookie.Value

	cipher, err := base64.StdEncoding.DecodeString(b64)
	if err != nil {
		return "", err
	}

	dataB, err := decrypt(cipher, secret[:])
	if err != nil {
		return "", err
	}
	data := string(dataB)

	split := strings.Index(data, "|")
	if split == -1 {
		return "", errors.New("invalid cookie")
	}

	issued, err := time.Parse(time.RFC3339, data[:split])
	if err != nil {
		return "", errors.New("invalid cookie")
	}

	if time.Since(issued) > allowedLifeTime {
		return "", errors.New("expired cookie")
	}

	return data[split+1:], nil
}

func encrypt(plaintext []byte, key []byte) ([]byte, error) {
	c, err := aes.NewCipher(key)
	if err != nil {
		return nil, err
	}

	gcm, err := cipher.NewGCM(c)
	if err != nil {
		return nil, err
	}

	nonce := make([]byte, gcm.NonceSize())
	if _, err = io.ReadFull(rand.Reader, nonce); err != nil {
		return nil, err
	}

	return gcm.Seal(nonce, nonce, plaintext, nil), nil
}

func decrypt(ciphertext []byte, key []byte) ([]byte, error) {
	c, err := aes.NewCipher(key)
	if err != nil {
		return nil, err
	}

	gcm, err := cipher.NewGCM(c)
	if err != nil {
		return nil, err
	}

	nonceSize := gcm.NonceSize()
	if len(ciphertext) < nonceSize {
		return nil, errors.New("ciphertext too short")
	}

	nonce, ciphertext := ciphertext[:nonceSize], ciphertext[nonceSize:]
	return gcm.Open(nil, nonce, ciphertext, nil)
}
