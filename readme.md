# Zephyr METAR API
This is a simple API that provides METAR for a given ICAO code. 
The METAR data is refreshed in regular intervals. 

## API Endpoints
Run the dev server using `dotnet run`. Visit `/swagger/index.html` to see the API documentation.
Endpoints are /Metar/Latest/{icao} and /Metar/Average/{icao}.