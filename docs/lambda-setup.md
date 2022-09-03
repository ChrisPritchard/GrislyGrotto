# AWS Lambda setup

In AWS:

- a vpc with a public and private subnet
- add a lambda to the vpc with access to both subnets
- create an efs store in the private subnet, add an access point
- in the lambda, add the 'AmazonElasticFileSystemClientFullAccess' managed role to its iam role assignments
- also needs s3 access rights, put and get on objects as well as bucket list
- add the efs access point under the lambda's storage tab
- also need a vpc endpoint to access s3

Next steps:

- [x] a means to upload and download data to the efs store (second lambda maybe? or swap in code)
- [x] rights to access s3
- [x] gg modified to not use creds for s3, and just to access using its iam permissions
  - this is not a gg thing in theory as the s3 session object is instantiated without config
  - it currently reads from env vars, but in the target env might 'just work'.
- [x] current flags replaced with or augmented with env vars
  - done
- export all aws config as cloudformation scripts
  - or terraform

Final issues:

- [x] glibc on the lambda
  - replaced with non-cgo sqlite handler
- [x] fix for malformed images
  - needed to base64 lambda response
- [x] non-random secret - in lambda this will be screwed everytime
- [x] logout fix
- [x] timezone fixing
- [-] writable database?
  - this wasnt an issue - the ip check was breaking it
- [x] open database when used, not on every request
- [ ] better logging
- [ ] looks like db performance adds 400 milliseconds or more... some sort of faster db solution should be investigated
- [ ] login brute forcing protection
- [ ] comment spam protection

- [ ] update about page
- [ ] update readmes

Hosting:

to have the custom domain, it needs to be fronted with a cloud front distro
then the cloud front has the alternate domain name - however this requires that the cert for the domain be uploaded
lets encrypt needs to renew the cert
a records need to be pointed to the cloudfront distro
  - this doesnt seem possible with a third party domain, due to being unable to point A records at cloud front