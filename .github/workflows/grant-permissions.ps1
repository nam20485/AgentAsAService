$SERVICEACCOUNTEMAIL = "firebase-adminsdk-xxxxx@agent-as-a-service-459620.iam.gserviceaccount.com"

# Grant Cloud Storage Admin
# gcloud projects add-iam-policy-binding agent-as-a-service-459620 `
#  --member "serviceAccount:<SERVICEACCOUNTEMAIL>" `
#  --role "roles/storage.admin"

# Grant Cloud Build Editor
# gcloud projects add-iam-policy-binding agent-as-a-service-459620 `
#  --member "serviceAccount:<SERVICEACCOUNTEMAIL>" `
#  --role "roles/cloudbuild.builds.editor"

# Grant Service Usage Consumer
gcloud projects add-iam-policy-binding agent-as-a-service-459620 `
  --member "serviceAccount:<$SERVICEACCOUNTEMAIL>" `
  --role "roles/serviceusage.serviceUsageConsumer"