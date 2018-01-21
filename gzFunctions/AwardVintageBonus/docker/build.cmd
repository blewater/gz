docker run -it --rm --volume `pwd`:/sc fsharpcanopy bash -c "cd /sc; mono .paket/paket.exe update; fsharpc awardbonus.fsx"
