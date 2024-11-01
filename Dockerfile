FROM public.ecr.aws/lambda/nodejs:20.2024.10.16.12
WORKDIR /app
COPY ./package.json /app/package.json
RUN npm install
COPY ./server.js /app/server.js

ENTRYPOINT [ "node","/app/server.js" ]
