#ifndef SONGREPOSITORY_H_
#define SONGREPOSITORY_H_

#include <vector>

#include <mysql/mysql.h>

#include "database/base_repository.h"
#include "models/models.h"
#include "types/songFilter.h"

class songRepository : public base_repository
{
public:
    songRepository(const std::string&);

    Song retrieveRecord(Song&, songFilter);

    void saveRecord(const Song&);
private:
    std::vector<Song> parseRecords(MYSQL_RES*);
    Song parseRecord(MYSQL_RES*);
};

#endif
