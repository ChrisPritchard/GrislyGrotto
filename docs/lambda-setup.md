# AWS Lambda setup

In AWS:

- a vpc with a public and private subnet
- add a lambda to the vpc with access to both subnets
- create an efs store in the private subnet, add an access point
- in the lambda, add the 'AmazonElasticFileSystemClientFullAccess' managed role to its iam role assignments
- add the efs access point under the lambda's storage tab

Next steps:

- a means to upload and download data to the efs store (second lambda maybe? or swap in code)
- rights to access s3
- gg modified to not use creds for s3, and just to access using its iam permissions
  - this is not a gg thing in theory as the s3 session object is instantiated without config
  - it currently reads from env vars, but in the target env might 'just work'.
- current flags replaced with or augmented with env vars
  - done
- export all aws config as cloudformation scripts
  - or terraform

Final issues:

- glibc on the lambda
- timezone setting