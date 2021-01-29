#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/sdk:3.1

WORKDIR /webapp

COPY /webapp /webapp

RUN mv /webapp/wwwroot/assets/uploads /webapp/volume_data/

RUN ln -s /webapp/volume_data/uploads/ /webapp/wwwroot/assets/uploads

EXPOSE 23516/tcp

ENV ASPNETCORE_URLS http://*:23516

ENTRYPOINT ["dotnet", "BakingBuddy.dll"]