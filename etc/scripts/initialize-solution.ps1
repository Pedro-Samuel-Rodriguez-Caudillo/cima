abp install-libs

cd src/cima.DbMigrator && dotnet run && cd -

cd src/cima.Blazor && dotnet dev-certs https -v -ep openiddict.pfx -p 5287ce65-c75c-4274-aece-cdada2a859d2




exit 0