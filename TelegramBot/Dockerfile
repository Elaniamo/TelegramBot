#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["TelegramBot/TelegramBot.csproj", "TelegramBot/"]
RUN dotnet restore "TelegramBot/TelegramBot.csproj"
COPY . .
WORKDIR "/src/TelegramBot"
RUN dotnet build "TelegramBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TelegramBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

 RUN apt-get update \
 && apt-get install -y locales \
 && sed -i '/ru_UA.UTF-8/s/^# //g' /etc/locale.gen \
 && locale-gen \
 && apt-get clean all
# && apt-get install -y locales locales-all \
# && apt-get install -y procps
# # ENV LANG en_US.UTF-8  
# # ENV LANGUAGE en_US:en  
# # ENV LC_ALL en_US.UTF-8  

 ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
 ENV TZ=Europe/Kiev
 ENV LANG=ru_UA.UTF-8
 ENV LANGUAGE=ru_UA.UTF-8
 ENV LC_ALL=ru_UA.UTF-8

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TelegramBot.dll"] 
