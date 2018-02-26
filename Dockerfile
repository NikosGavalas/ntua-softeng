
FROM	mono:5.4.1.6

ENV		PATH="/opt:${PATH}"


COPY 	./Pleisure/bin/Debug /opt/pleisure


WORKDIR /opt/pleisure


CMD		[ "mono", "Pleisure.exe" ] 