# AWS Lambda setup

Hosting in AWS lambda uses the site-lambda-host cmd project - just compile as normal, then zip and upload to the lambda destination.

The setup in AWS is roughly:

- a vpc with a public and private subnet
- add a lambda to the vpc with access to both subnets (env should be Go, and entrypoint main)
- create an efs store in the private subnet, add an access point
- in the lambda, add the 'AmazonElasticFileSystemClientFullAccess' managed role to its iam role assignments
- also needs s3 access rights, put and get on objects as well as bucket list
- add the efs access point under the lambda's storage tab
- also need a vpc endpoint to access s3

That is, the lambda accesses the database on an EFS mount (at /mnt/efs generally) and content on S3.

For a custom domain, this works best using Route 53 managed domains.

Under the [lambda tools](../cmd/lambda-tools/) folder are various projects that can be used to help test and deploy into lambda:

- efs-uploader will list and allow the creation of files on the efs mount - as EFS doesnt have an interface like S3, this can be required to put things like a database in there. However, this only supports files of max size 6MB or so, therefore...
- the file-splitter will break files into chunks. This can be used with the efs-uploader, which supports appending chunks to a file, to upload large files like the ~25mb database
- s3-uploader lists and uploads/deletes s3 content. its primary purpose is testing the lambda can reach s3 and edit successfully. files can be read using generated temporary links.
- sql-tester allows queries to be run against a sqlite database, and again is primarily for testing.

Each project relies on certain environment variables to function, such as ACCESSKEY for most of the non-site lambda projects. Reviewing the source helps identify these.