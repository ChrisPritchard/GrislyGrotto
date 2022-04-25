#!/usr/bin/env bash

# note this is for my own use, and won't work for other machines.
# however can be interesting as an example of a hacky little release script

env CGO_ENABLED=1 GOOS=linux GOARCH=amd64 go build -o grislygrotto.so ../cmd/site/main.go
scp -i ~/grislygrotto-web-aws.pem grislygrotto.so ubuntu@ec2-52-62-165-198.ap-southeast-2.compute.amazonaws.com:
ssh -i ~/grislygrotto-web-aws.pem ubuntu@ec2-52-62-165-198.ap-southeast-2.compute.amazonaws.com "sudo systemctl stop grislygrotto.service && sudo mv ~/grislygrotto.so /var/www/grislygrotto/ && sudo systemctl start grislygrotto.service"
rm grislygrotto.so