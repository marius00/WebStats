# WebStats

## Adding a new project  

`INSERT INTO services(description,shortname, project_url) VALUES ('Grim Dawn Item Assistant', 'iagd', 'https://website-name-or-null.example.com')`

# Viewing stats  
Simply visit `/shortname`

# Reported versions  
Only msbuild timestamp versions are stored, ex: `1.5.9732.15787`.

A client reporting todays date as its version is a third party build. Those keep no persistent uuid,
so every launch would count as another user. They are ignored entirely rather than counted.


The container listens on port 5000 and runs as a non-root user. `GET /health` returns 200 `Healthy`
while the app is alive.