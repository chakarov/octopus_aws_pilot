# octopus_aws_pilot

This is used internally here to achieve the following flow:

- Upon a successful Team City build, the app is invoked in the final build step of the final build proj in our dependency chain

- The app provision a vm with a given AMI on AWS

- The app then creates a release on octopus

- The app then creates a deploy on octopus, with the specific machine ip (we use elastic ip to have a fixed set of ips)

- In our case, one of the deployment steps is to run our integration tests

- Once the deployment is done, we report success or failure to TC, and terminate the instance


Note this holds up a build agent for the whole process (may take up to 30 minutes to provision and deploy your app)


