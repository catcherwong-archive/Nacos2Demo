docker build -t nacos2demo:naming -f .\docker\dockerfile.config .

docker run --rm --name nacos2naming -p 9555:9555 nacos2demo:config