FROM ubuntu:14.04

# That's me.
MAINTAINER Manolito Snell

# We need XVFB to actually being able to render to a headless server.
RUN apt-get update && apt-get install -y git libglu1 xvfb libxcursor1

# Copy assets.
COPY AssetRender.x86_64 /etc/asset-render/
COPY AssetRender_Data /etc/asset-render/AssetRender_Data/

# Open up the exposed port.
EXPOSE 4000

# Run the AssetRender with a Virtual Frame Buffer.
# Building the Application from within Unity with 'headless' ticked will prevent rendering at all.
# Also do not use '-batchmode' or 'no-graphics'.
CMD xvfb-run --auto-servernum --server-args='-screen 0 640x480x24:32' /etc/asset-render/AssetRender.x86_64 -logFile /var/log/asset-render