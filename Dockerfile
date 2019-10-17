FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-disco as net-builder

ARG IsProduction=false
ARG CiCommitName=local
ARG CiCommitHash=sha

WORKDIR /build
ADD src .
RUN dotnet restore

WORKDIR /build/

ENV CiCommitName=$CiCommitName

RUN dotnet publish --output out/ --configuration Release --runtime linux-x64 --self-contained true ImageBoard

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100-disco
ENV TZ=Europe/Moscow
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

WORKDIR /app
COPY --from=net-builder /build/out ./net

ADD run.sh .
RUN chmod +x run.sh

ARG CiCommitName=local
ARG CiCommitHash=sha
ARG IsProduction=false
ENV Properties__IsProduction=$IsProduction
ENV Properties__CiCommitName=$CiCommitName
ENV Properties__CiCommitHash=$CiCommitHash
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/app/run.sh"]
