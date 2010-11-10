

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:3 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/m.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = exe
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug/

M_EXE_MDB_SOURCE=bin/Debug/m.exe.mdb
M_EXE_MDB=$(BUILD_DIR)/m.exe.mdb
RHINO_MOCKS_DLL_SOURCE=bin/Rhino.Mocks.dll
GSTREAMER_SHARP_DLL_SOURCE=bin/Debug/gstreamer-sharp.dll
GALLIO_DLL_SOURCE=bin/Debug/Gallio.dll
GSTREAMER_SHARP_DLL_CONFIG_SOURCE=bin/Debug/gstreamer-sharp.dll.config
TAO_OPENAL_DLL_SOURCE=bin/Tao.OpenAl.dll
MBUNIT_DLL_SOURCE=bin/Debug/MbUnit.dll
TAO_FFMPEG_DLL_SOURCE=bin/Tao.FFmpeg.dll
MBUNIT_COMPATIBILITY_DLL_SOURCE=bin/Debug/MbUnit.Compatibility.dll

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = gmcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE"
ASSEMBLY = bin/Release/m.exe
ASSEMBLY_MDB = 
COMPILE_TARGET = exe
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release/

M_EXE_MDB=
RHINO_MOCKS_DLL_SOURCE=bin/Rhino.Mocks.dll
GSTREAMER_SHARP_DLL_SOURCE=bin/Debug/gstreamer-sharp.dll
GALLIO_DLL_SOURCE=bin/Debug/Gallio.dll
GSTREAMER_SHARP_DLL_CONFIG_SOURCE=bin/Debug/gstreamer-sharp.dll.config
TAO_OPENAL_DLL_SOURCE=bin/Tao.OpenAl.dll
MBUNIT_DLL_SOURCE=bin/Debug/MbUnit.dll
TAO_FFMPEG_DLL_SOURCE=bin/Tao.FFmpeg.dll
MBUNIT_COMPATIBILITY_DLL_SOURCE=bin/Debug/MbUnit.Compatibility.dll

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(M_EXE_MDB) \
	$(RHINO_MOCKS_DLL) \
	$(GSTREAMER_SHARP_DLL) \
	$(GALLIO_DLL) \
	$(GSTREAMER_SHARP_DLL_CONFIG) \
	$(TAO_OPENAL_DLL) \
	$(MBUNIT_DLL) \
	$(TAO_FFMPEG_DLL) \
	$(MBUNIT_COMPATIBILITY_DLL)  

BINARIES = \
	$(M)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(BINARIES) 

FILES = \
	FFMpegAudioStream.cs \
	AudioStreamFactory.cs \
	BackgroundWorkerFactory.cs \
	BackgroundWorkerWrapper.cs \
	Command.cs \
	CommandFactory.cs \
	ConfigSettingsFacade.cs \
	ConsoleFacade.cs \
	Decoder.cs \
	FileFinder.cs \
	FilenameComparer.cs \
	FileSystemFacade.cs \
	IAudioStream.cs \
	IAudioStreamFactory.cs \
	IBackgroundWorkerFactory.cs \
	IBackgroundWorkerWrapper.cs \
	ICommand.cs \
	ICommandDiscriminator.cs \
	ICommandFactory.cs \
	IConfigSettingsFacade.cs \
	IConsoleFacade.cs \
	IFileFinder.cs \
	IFileSystemFacade.cs \
	IInformationDisplayer.cs \
	InformationDisplayer.cs \
	IPlaylistReader.cs \
	ISearchQuery.cs \
	ISearchQueryFactory.cs \
	IStateMachine.cs \
	ITextDiscriminator.cs \
	mConfig.cs \
	Player.cs \
	PlaylistReader.cs \
	Program.cs \
	Properties/AssemblyInfo.cs \
	SearchQuery.cs \
	SearchQueryFactory.cs \
	tests/AudioStreamFactoryFixture.cs \
	tests/BackgroundWorkerFactoryFixture.cs \
	tests/CommandFactoryFixture.cs \
	tests/FileFinderFixture.cs \
	tests/FilenameComparerFixture.cs \
	tests/InformationDisplayerFixture.cs \
	tests/mConfigFixture.cs \
	tests/MockingFixture.cs \
	tests/PlayerFixture.cs \
	tests/PlaylistReaderFixture.cs \
	tests/ProgramFixture.cs \
	tests/SearchQueryFactoryFixture.cs \
	tests/SearchQueryFixture.cs \
	tests/TextDiscriminatorFixture.cs \
	TextDiscriminator.cs \
	GStreamerAudioStream.cs \
	Platform.cs \
	IPlatform.cs 

DATA_FILES = 

RESOURCES = \
	texts/help,m.texts.help \
	tests/playlists/m3u,m.tests.playlists.m3u \
	tests/playlists/pls,m.tests.playlists.pls \
	tests/playlists/plaintext,m.tests.playlists.plaintext \
	tests/playlists/m3uwithurl,m.tests.playlists.m3uwithurl 

EXTRAS = \
	app.config \
	m.in 

REFERENCES =  \
	System \
	System.Configuration \
	System.Data \
	System.Xml \
	$(GLIB_SHARP_20_LIBS)

DLL_REFERENCES =  \
	bin/Rhino.Mocks.dll \
	bin/Tao.FFmpeg.dll \
	bin/Tao.OpenAl.dll \
	bin/Debug/Gallio.dll \
	bin/Debug/MbUnit.Compatibility.dll \
	bin/Debug/MbUnit.dll \
	bin/Debug/gstreamer-sharp.dll

CLEANFILES = $(PROGRAMFILES) $(BINARIES) 

include $(top_srcdir)/Makefile.include

RHINO_MOCKS_DLL = $(BUILD_DIR)/Rhino.Mocks.dll
GSTREAMER_SHARP_DLL = $(BUILD_DIR)/gstreamer-sharp.dll
M = $(BUILD_DIR)/m
GALLIO_DLL = $(BUILD_DIR)/Gallio.dll
GSTREAMER_SHARP_DLL_CONFIG = $(BUILD_DIR)/gstreamer-sharp.dll.config
TAO_OPENAL_DLL = $(BUILD_DIR)/Tao.OpenAl.dll
MBUNIT_DLL = $(BUILD_DIR)/MbUnit.dll
TAO_FFMPEG_DLL = $(BUILD_DIR)/Tao.FFmpeg.dll
MBUNIT_COMPATIBILITY_DLL = $(BUILD_DIR)/MbUnit.Compatibility.dll

$(eval $(call emit-deploy-target,RHINO_MOCKS_DLL))
$(eval $(call emit-deploy-target,GSTREAMER_SHARP_DLL))
$(eval $(call emit-deploy-wrapper,M,m,x))
$(eval $(call emit-deploy-target,GALLIO_DLL))
$(eval $(call emit-deploy-target,GSTREAMER_SHARP_DLL_CONFIG))
$(eval $(call emit-deploy-target,TAO_OPENAL_DLL))
$(eval $(call emit-deploy-target,MBUNIT_DLL))
$(eval $(call emit-deploy-target,TAO_FFMPEG_DLL))
$(eval $(call emit-deploy-target,MBUNIT_COMPATIBILITY_DLL))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
