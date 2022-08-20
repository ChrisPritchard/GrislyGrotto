# Cmd Functions

The two main entry points are [lambda](./lambda/) and [site](./site/) - which will run GrislyGrotto as a AWS Lambda or as a standard self-serving website respectively.

[Authors](./authors/) allows initialising a database with usernames and hashed passwords in a format the site can read.

[EFS Uploader](./efs-uploader/) is designed to be run as a lambda that is connected to an EFS access point, and allows uploading and deleting files from EFS (this is otherwise difficult without an EC2 instance).

[File splitter](./efs-uploader/) can split any file into chunks of a given size. Its intended use is with the EFS Uploader, which being a lambda has a maximum invocation size of 6MB. The uploader can append data to existing files however, so the splitter works with this to get large files up (basically the database, which at time of writing was 25mb).