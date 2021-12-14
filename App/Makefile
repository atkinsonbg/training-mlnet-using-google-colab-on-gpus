build:
	dotnet build -o BuildOutput

train:
	./BuildOutput/App --train --folder="../App/Data/LocalImages" --file="training_data.csv"

build-colab: clean-colab
	dotnet publish -o ColabOutput -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true /p:DebugType=None /p:DebugSymbols=false

clean-colab:
	rm -rf ColabOutput