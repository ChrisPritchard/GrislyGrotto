# AWS Lambda setup

In AWS:

- a vpc with a public and private subnet
- add a lambda to the vpc with access to both subnets
- create an efs store in the private subnet, add an access point
- in the lambda, add the 'AmazonElasticFileSystemClientFullAccess' managed role to its iam role assignments
- add the efs access point under the lambda's storage tab

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
- [ ] non-random secret - in lambda this will be screwed everytime
- [ ] timezone fixing
- [ ] writable database?
- [ ] better logging
- [ ] update about page
- [ ] update readmes
