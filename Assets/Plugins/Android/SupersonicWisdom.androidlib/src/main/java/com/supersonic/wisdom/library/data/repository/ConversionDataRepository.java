package com.supersonic.wisdom.library.data.repository;

import com.supersonic.wisdom.library.data.repository.datasource.ConversionDataLocalDataSource;
import com.supersonic.wisdom.library.domain.events.IConversionDataRepository;

public class ConversionDataRepository implements IConversionDataRepository {

    private ConversionDataLocalDataSource mDataSource;

    public ConversionDataRepository(ConversionDataLocalDataSource dataSource) {
        mDataSource = dataSource;
    }

    @Override
    public String getConversionData() {
        return mDataSource.getConversionData();
    }
}
