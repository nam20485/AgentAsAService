
# install wasm-tools workload
sudo dotnet workload install wasm-tools

& ([scriptblock]::Create((iwr 'https://to.loredo.me/Install-NerdFont.ps1'))) -Confirm:$false -Name hack, heavy-data

# install LTS version of node/npm
nvm install --lts
nvm use --lts

# update gcloud CLI
gcloud components update --quiet

# update firebase CLI
npm install -g firebase-tools

# check CLI commands working
nvm --version
npm --version
node --version
gh --version
gcloud --version
firebase --version
