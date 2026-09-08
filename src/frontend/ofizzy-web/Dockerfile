FROM node:24-alpine AS development
WORKDIR /app
COPY src/frontend/ofizzy-web/package*.json ./
RUN npm ci
COPY src/frontend/ofizzy-web .
EXPOSE 4200
CMD ["npm", "start", "--", "--host", "0.0.0.0", "--poll", "1000"]

FROM development AS build
RUN npm run build

FROM nginx:stable-alpine AS runtime
COPY deploy/nginx/frontend.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/ofizzy-web/browser /usr/share/nginx/html
EXPOSE 8080
