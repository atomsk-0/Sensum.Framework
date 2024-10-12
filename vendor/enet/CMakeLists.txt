cmake_minimum_required(VERSION 3.25.1)
project(enet)

set(CMAKE_CXX_STANDARD 17)

if(MSVC)
    add_definitions(-W3)
else()
    add_definitions(-Wno-error)
endif()

add_library(enet SHARED enet.cpp)

target_compile_definitions(enet PRIVATE
        ENET_DLL
        NDEBUG
)