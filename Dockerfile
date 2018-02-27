FROM	mono:5.8.0.108

ENV		PATH="/opt:${PATH}"


COPY 	./Pleisure/bin/Debug /opt/pleisure


WORKDIR /opt/pleisure


CMD		[ "mono", "Pleisure.exe" ] 