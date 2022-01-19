import Koa from 'koa';
import axios from 'axios';

// How we connect to the dotnet service with dapr
const daprSidecarBaseUrl = `http://localhost:${process.env.DAPR_HTTP_PORT || 3501}`
// app id header for service discovery
const weatherServiceAppIdHeaders = {
    'dapr-app-id': process.env.WEATHER_SERVICE_NAME || 'dotnet-app'
};

const app = new Koa();

app.use(async ctx => {
    try {
        const data = await axios.get<WeatherForecast[]>(`${daprSidecarBaseUrl}/weatherForecast`, {
            headers: weatherServiceAppIdHeaders
        });

        ctx.body = `And the weather today will be ${data.data[0].summary}`;
    } catch (exc) {
        console.error('Problem calling weather service', exc)
        ctx.body = 'Something went wrong!'
    }
});

const portNumber = 3000;
app.listen(portNumber);
console.log(`listening on port ${portNumber}`);

interface WeatherForecast {
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}
